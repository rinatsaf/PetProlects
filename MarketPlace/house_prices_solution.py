# ============================================================
# HOUSE PRICES (Kaggle Learn) — решение для топ-лидерборда
# Метрика соревнования: MAE. Поэтому все модели обучаем с L1-лоссом.
# Работает на Kaggle без интернета и GPU.
# ------------------------------------------------------------
# ДОБАВЛЕНО:
#   1) АНАЛИЗ MUTUAL INFORMATION по существующим признакам (сначала).
#   2) FEATURE ENGINEERING по результатам MI — новые признаки строим
#      вокруг самых информативных драйверов цены.
#   3) ВИЗУАЛИЗАЦИЯ (MI, корреляция новых фич, scatter, гистограммы,
#      флаги, важность). Все графики через plt.show().
# ============================================================

import numpy as np
import pandas as pd
from sklearn.model_selection import KFold
from sklearn.metrics import mean_absolute_error
from scipy.optimize import minimize
import lightgbm as lgb
from catboost import CatBoostRegressor
from xgboost import XGBRegressor

import glob
import matplotlib.pyplot as plt
import seaborn as sns
sns.set_theme(style="whitegrid", palette="viridis")

# Автоматически находим пути к train.csv и test.csv внутри /kaggle/input
_train_candidates = glob.glob('/kaggle/input/**/train.csv', recursive=True)
_test_candidates  = glob.glob('/kaggle/input/**/test.csv', recursive=True)
if not _train_candidates or not _test_candidates:
    raise FileNotFoundError('Добавь датасет: Add Input → home-data-for-ml-course')

# Предпочитаем файл из home-data-for-ml-course, иначе берём первый найденный
train_path = next((p for p in _train_candidates if 'home-data' in p), _train_candidates[0])
test_path  = next((p for p in _test_candidates  if 'home-data' in p), _test_candidates[0])
print('train:', train_path)
print('test :', test_path)

train = pd.read_csv(train_path)
test  = pd.read_csv(test_path)
test_ids = test['Id'].copy()          # Id нужны для файла сабмита

# ------------------------------------------------------------
# 2. УДАЛЕНИЕ ВЫБРОСОВ
# В данных Ames есть 2 «частичные продажи»: очень большая площадь,
# но подозрительно низкая цена. Они сильно портят MAE — убираем.
# ------------------------------------------------------------
train = train.drop(
    train[(train.GrLivArea > 4000) & (train.SalePrice < 300000)].index
).reset_index(drop=True)
y = train['SalePrice'].values         # целевая переменная
n_train = len(train)                  # размер train до one-hot (нужен для визуализации)

# Объединяем train и test, чтобы категории кодировались одинаково
all_data = pd.concat(
    [train.drop('SalePrice', axis=1), test.drop('Id', axis=1)],
    axis=0, ignore_index=True
)

# ------------------------------------------------------------
# 3. ЗАПОЛНЕНИЕ ПРОПУСКОВ ПО СМЫСЛУ
# В этом датасете NA часто означает «удобства нет», а не «данные утеряны».
# ------------------------------------------------------------
none_cols = ['Alley', 'BsmtQual', 'BsmtCond', 'BsmtExposure', 'BsmtFinType1',
             'BsmtFinType2', 'FireplaceQu', 'GarageType', 'GarageFinish',
             'GarageQual', 'GarageCond', 'PoolQC', 'Fence', 'MiscFeature']
for c in none_cols:
    all_data[c] = all_data[c].fillna('None')      # нет балкона/бассейна/гаража и т.п.

# Год постройки гаража неизвестен -> берём год постройки дома
all_data['GarageYrBlt'] = all_data['GarageYrBlt'].fillna(all_data['YearBuilt'])

# Фронт участка: заполняем медианой по району (похожие дома рядом)
all_data['LotFrontage'] = all_data.groupby('Neighborhood')['LotFrontage'].transform(
    lambda s: s.fillna(s.median())
)
all_data['LotFrontage'] = all_data['LotFrontage'].fillna(all_data['LotFrontage'].median())

# Площади/количества, отсутствие которых = 0
for c in ['BsmtFinSF1', 'BsmtFinSF2', 'BsmtUnfSF', 'TotalBsmtSF', 'BsmtFullBath',
          'BsmtHalfBath', 'GarageCars', 'GarageArea', 'MasVnrArea']:
    all_data[c] = all_data[c].fillna(0)

# Категориальные с малым числом пропусков -> самое частое значение
for c in ['MSZoning', 'Electrical', 'KitchenQual', 'Exterior1st', 'Exterior2nd',
          'SaleType', 'Functional', 'Utilities']:
    all_data[c] = all_data[c].fillna(all_data[c].mode()[0])

# Страховка: в числовых столбцах не должно остаться NaN
num_cols = all_data.select_dtypes(include=[np.number]).columns
all_data[num_cols] = all_data[num_cols].fillna(all_data[num_cols].median())

# ------------------------------------------------------------
# 4. ПОРЯДКОВОЕ КОДИРОВАНИЕ КАЧЕСТВЕННЫХ ШКАЛ
# Порядок Ex>Gd>TA>Fa>Po важен для модели — сохраняем его числами.
# ------------------------------------------------------------
qual = {'Ex': 5, 'Gd': 4, 'TA': 3, 'Fa': 2, 'Po': 1, 'None': 0}
for c in ['ExterQual', 'ExterCond', 'BsmtQual', 'BsmtCond', 'HeatingQC',
          'KitchenQual', 'FireplaceQu', 'GarageQual', 'GarageCond', 'PoolQC']:
    all_data[c] = all_data[c].map(qual).fillna(0).astype(int)

all_data['BsmtExposure'] = all_data['BsmtExposure'].map(
    {'Gd': 4, 'Av': 3, 'Mn': 2, 'No': 1, 'None': 0}).fillna(0).astype(int)

fin = {'GLQ': 6, 'ALQ': 5, 'BLQ': 4, 'Rec': 3, 'LwQ': 2, 'Unf': 1, 'None': 0}
for c in ['BsmtFinType1', 'BsmtFinType2']:
    all_data[c] = all_data[c].map(fin).fillna(0).astype(int)

all_data['GarageFinish'] = all_data['GarageFinish'].map(
    {'Fin': 3, 'RFn': 2, 'Unf': 1, 'None': 0}).fillna(0).astype(int)

all_data['Functional'] = all_data['Functional'].map(
    {'Typ': 7, 'Min1': 6, 'Min2': 5, 'Mod': 4, 'Maj1': 3, 'Maj2': 2,
     'Sev': 1, 'Sal': 0}).fillna(7).astype(int)

all_data['Fence'] = all_data['Fence'].map(
    {'GdPrv': 4, 'MnPrv': 3, 'GdWo': 2, 'MnWw': 1, 'None': 0}).fillna(0).astype(int)

all_data['CentralAir'] = (all_data['CentralAir'] == 'Y').astype(int)
all_data['PavedDrive'] = all_data['PavedDrive'].map(
    {'Y': 2, 'P': 1, 'N': 0}).fillna(0).astype(int)

# ------------------------------------------------------------
# 5. АНАЛИЗ MUTUAL INFORMATION ПО СУЩЕСТВУЮЩИМ ПРИЗНАКАМ
# Сначала смотрим, какие ИЗНАЧАЛЬНЫЕ признаки несут больше всего
# информации о цене. От них отталкиваемся при создании новых фич.
# ------------------------------------------------------------
from sklearn.feature_selection import mutual_info_regression

mi_df = all_data.iloc[:n_train].copy()          # только train-часть
cat_cols_mi = [c for c in mi_df.columns if mi_df[c].dtype == 'object']
for c in cat_cols_mi:
    mi_df[c] = mi_df[c].astype('category').cat.codes

mi_values = mutual_info_regression(
    mi_df.values, y,
    discrete_features=[c in cat_cols_mi for c in mi_df.columns],
    random_state=42, n_neighbors=5,
)
mi_scores = pd.Series(mi_values, index=mi_df.columns, name='MI').sort_values(ascending=False)

print("\nТоп-15 существующих признаков по Mutual Information:")
print(mi_scores.head(15).round(4).to_string())

# График: MI всех существующих признаков (красным — лидер)
plt.figure(figsize=(12, max(8, len(mi_scores) * 0.22)))
top_val = mi_scores.iloc[0]
plt.barh(mi_scores.index[::-1], mi_scores.values[::-1],
         color=['#c0392b' if v == top_val else '#3498db' for v in mi_scores.values[::-1]])
plt.xlabel('Mutual Information (биты)')
plt.title('Mutual Information существующих признаков с SalePrice')
plt.tight_layout()
plt.show()

top_mi = mi_scores.head(10).index.tolist()
print("Признаки-драйверы (топ-10) для инженерии новых фич:", top_mi)

# ------------------------------------------------------------
# 6. FEATURE ENGINEERING (по результатам MI)
# MI показал: цену сильнее всего определяют OverallQual, GrLivArea,
# Neighborhood, GarageCars, TotalBsmtSF, YearBuilt и качественные шкалы.
# Поэтому новые признаки строим именно вокруг этих драйверов.
# Все новые колонки копим в словарь и добавляем одним pd.concat(axis=1),
# чтобы не плодить предупреждения о фрагментации DataFrame.
# ------------------------------------------------------------

# Промежуточные величины, на которые опираются другие признаки
total_sf   = all_data['TotalBsmtSF'] + all_data['1stFlrSF'] + all_data['2ndFlrSF']
total_bath = (all_data['FullBath'] + 0.5 * all_data['HalfBath']
              + all_data['BsmtFullBath'] + 0.5 * all_data['BsmtHalfBath'])
total_porch = (all_data['OpenPorchSF'] + all_data['EnclosedPorch']
               + all_data['3SsnPorch'] + all_data['ScreenPorch'])
total_rms  = all_data['TotRmsAbvGrd'] + all_data['BedroomAbvGr']

fe = {}

# ---- Способ 1: СУММЫ / ОБЪЕДИНЕНИЯ (целостный размер дома) ----
fe['TotalSF']      = total_sf
fe['TotalBath']    = total_bath
fe['TotalPorchSF'] = total_porch
fe['TotalRms']     = total_rms
fe['TotalOutdoor'] = all_data['WoodDeckSF'] + total_porch + all_data['PoolArea']     # вся внешняя зона
fe['TotalFinSF']   = all_data['GrLivArea'] + all_data['BsmtFinSF1'] + all_data['BsmtFinSF2']  # вся отделанная площадь

# ---- Способ 2: ОТНОШЕНИЯ / ПЛОТНОСТИ (эффективность площади) ----
fe['LivingPerRoom']    = all_data['GrLivArea'] / (all_data['TotRmsAbvGrd'] + 1)      # площадь на комнату
fe['BathPerRoom']      = total_bath / (total_rms + 1)                                # санузлов на комнату
fe['AreaPerRoom']      = total_sf / (total_rms + 1)                                  # общей площади на комнату
fe['GarageAreaPerCar'] = all_data['GarageArea'] / (all_data['GarageCars'] + 1)       # площадь гаража на машину
fe['BsmtRatio']        = all_data['TotalBsmtSF'] / (total_sf + 1)                    # доля подвала
fe['PorchRatio']       = total_porch / (total_sf + 1)                                # доля террас
fe['BedroomRatio']     = all_data['BedroomAbvGr'] / (all_data['TotRmsAbvGrd'] + 1)   # доля спален
fe['LotFrontageRatio'] = all_data['LotFrontage'] / (all_data['LotArea'] + 1)         # вытянутость участка

# ---- Способ 3: ПРОИЗВЕДЕНИЯ / ВЗАИМОДЕЙСТВИЯ (качество × размер) ----
fe['QualArea']     = all_data['OverallQual'] * all_data['GrLivArea']     # качество, взвешенное на площадь
fe['QualTotalSF']  = all_data['OverallQual'] * total_sf                  # качество × общая площадь
fe['QualBath']     = all_data['OverallQual'] * total_bath                # качество × санузлы
fe['QualGarage']   = all_data['OverallQual'] * all_data['GarageCars']    # качество × вместимость гаража
fe['OverallGrade'] = all_data['OverallQual'] * all_data['OverallCond']   # качество × состояние

# ---- Способ 4: ВОЗРАСТ / ВРЕМЯ ----
fe['Age']         = all_data['YrSold'] - all_data['YearBuilt']            # возраст дома
fe['RemodAge']    = all_data['YrSold'] - all_data['YearRemodAdd']         # лет с ремонта
fe['AgeAtRemod']  = all_data['YearRemodAdd'] - all_data['YearBuilt']      # возраст на момент ремонта
fe['GarageAge']   = all_data['YrSold'] - all_data['GarageYrBlt']          # возраст гаража
fe['IsRemodeled'] = (all_data['YearRemodAdd'] != all_data['YearBuilt']).astype(int)  # был ли ремонт
fe['IsNew']       = (all_data['YrSold'] == all_data['YearBuilt']).astype(int)        # новый дом

# ---- Способ 5: БИНАРНЫЕ ФЛАГИ (есть/нет удобство) ----
fe['HasPool']      = (all_data['PoolArea'] > 0).astype(int)
fe['HasGarage']    = (all_data['GarageArea'] > 0).astype(int)
fe['HasBsmt']      = (all_data['TotalBsmtSF'] > 0).astype(int)
fe['HasFireplace'] = (all_data['Fireplaces'] > 0).astype(int)
fe['HasWoodDeck']  = (all_data['WoodDeckSF'] > 0).astype(int)
fe['HasPorch']     = (total_porch > 0).astype(int)
fe['Has2ndFloor']  = (all_data['2ndFlrSF'] > 0).astype(int)
fe['HasBsmtBath']  = ((all_data['BsmtFullBath'] + all_data['BsmtHalfBath']) > 0).astype(int)

# ---- Способ 6: МАТЕМАТИЧЕСКИЕ ПРЕОБРАЗОВАНИЯ (борьба с нелинейностью) ----
fe['LogGrLivArea']  = np.log1p(all_data['GrLivArea'])
fe['LogTotalSF']    = np.log1p(total_sf)
fe['LogLotArea']    = np.log1p(all_data['LotArea'])
fe['SqrtGrLivArea'] = np.sqrt(all_data['GrLivArea'])

# ---- Способ 7: АГРЕГАЦИИ ПО РАЙОНУ (контекст, БЕЗ утечки целевой) ----
neighborhood = all_data['Neighborhood']
fe['NbhdAvgSF']   = total_sf.groupby(neighborhood).transform('mean')                          # средняя площадь района
fe['NbhdAvgQual'] = all_data['OverallQual'].groupby(neighborhood).transform('mean')           # среднее качество района
fe['NbhdAvgLot']  = all_data['LotArea'].groupby(neighborhood).transform('mean')               # средний участок района
fe['NbhdCount']   = neighborhood.groupby(neighborhood).transform('count')                     # размер района

# Добавляем все новые признаки разом (без предупреждений о фрагментации)
all_data = pd.concat([all_data, pd.DataFrame(fe, index=all_data.index)], axis=1)

# ------------------------------------------------------------
# 6.4 MUTUAL INFORMATION: старые + новые признаки ВМЕСТЕ
# Пересчитываем MI уже с добавленными фичами. Новые красим оранжевым,
# старые — синим, чтобы сразу видеть, какие из них вошли в топ.
# ------------------------------------------------------------
mi_df2 = all_data.iloc[:n_train].copy()
cat_cols_mi2 = [c for c in mi_df2.columns if mi_df2[c].dtype == 'object']
for c in cat_cols_mi2:
    mi_df2[c] = mi_df2[c].astype('category').cat.codes

mi_values2 = mutual_info_regression(
    mi_df2.values, y,
    discrete_features=[c in cat_cols_mi2 for c in mi_df2.columns],
    random_state=42, n_neighbors=5,
)
mi_scores2 = pd.Series(mi_values2, index=mi_df2.columns, name='MI').sort_values(ascending=False)

new_feature_names = list(fe.keys())
import matplotlib.patches as mpatches
colors2 = ['#e67e22' if c in new_feature_names else '#3498db' for c in mi_scores2.index]

plt.figure(figsize=(12, max(10, len(mi_scores2) * 0.20)))
plt.barh(mi_scores2.index[::-1], mi_scores2.values[::-1], color=colors2[::-1])
plt.xlabel('Mutual Information (биты)')
plt.title('Mutual Information: старые (синие) + новые (оранжевые) признаки')
plt.legend(handles=[mpatches.Patch(color='#3498db', label='старые'),
                    mpatches.Patch(color='#e67e22', label='новые')])
plt.tight_layout()
plt.show()

print("\nТоп-15 признаков (старые + новые) по Mutual Information:")
print(mi_scores2.head(15).round(4).to_string())

# ------------------------------------------------------------
# 6.5 ВИЗУАЛИЗАЦИЯ СОЗДАННЫХ ПРИЗНАКОВ
# Рисуем ДО one-hot, пока у колонок человеческие имена.
# ------------------------------------------------------------
train_fe = all_data.iloc[:n_train].copy()   # train-часть с новыми признаками
y_vis = pd.Series(y)

new_features = [
    # суммы
    'TotalSF', 'TotalBath', 'TotalPorchSF', 'TotalRms', 'TotalOutdoor', 'TotalFinSF',
    # отношения
    'LivingPerRoom', 'BathPerRoom', 'AreaPerRoom', 'GarageAreaPerCar', 'BsmtRatio',
    'PorchRatio', 'BedroomRatio', 'LotFrontageRatio',
    # произведения
    'QualArea', 'QualTotalSF', 'QualBath', 'QualGarage', 'OverallGrade',
    # возраст
    'Age', 'RemodAge', 'AgeAtRemod', 'GarageAge', 'IsRemodeled', 'IsNew',
    # флаги
    'HasPool', 'HasGarage', 'HasBsmt', 'HasFireplace', 'HasWoodDeck', 'HasPorch',
    'Has2ndFloor', 'HasBsmtBath',
    # преобразования
    'LogGrLivArea', 'LogTotalSF', 'LogLotArea', 'SqrtGrLivArea',
    # агрегации района
    'NbhdAvgSF', 'NbhdAvgQual', 'NbhdAvgLot', 'NbhdCount',
]

# 1) Корреляция новых признаков с ценой (бар-чарт)
corr_new = pd.Series({c: train_fe[c].corr(y_vis) for c in new_features}).sort_values()
plt.figure(figsize=(10, 12))
colors = ['#c0392b' if v < 0 else '#3498db' for v in corr_new.values]
plt.barh(corr_new.index, corr_new.values, color=colors)
plt.xlabel('Корреляция Пирсона с SalePrice')
plt.title('Новые признаки: корреляция с ценой')
plt.tight_layout()
plt.show()

# 2) Scatter топ-4 новых признаков по модулю корреляции
top_corr = corr_new.abs().sort_values(ascending=False).head(4).index
fig, axes = plt.subplots(2, 2, figsize=(14, 10))
for ax, c in zip(axes.ravel(), top_corr):
    ax.scatter(train_fe[c], y, s=8, alpha=0.4)
    ax.set_xlabel(c)
    ax.set_ylabel('SalePrice')
    ax.set_title(f'{c} (r = {train_fe[c].corr(y_vis):.2f})')
plt.tight_layout()
plt.show()

# 3) Гистограммы распределения ключевых новых признаков
fig, axes = plt.subplots(2, 2, figsize=(14, 8))
for ax, c in zip(axes.ravel(), ['TotalSF', 'LivingPerRoom', 'QualArea', 'Age']):
    train_fe[c].hist(ax=ax, bins=40, color='#3498db')
    ax.set_title(f'Распределение: {c}')
plt.tight_layout()
plt.show()

# 4) Средняя цена в зависимости от флагов «есть/нет»
flags = ['HasPool', 'HasGarage', 'Has2ndFloor', 'HasFireplace']
fig, axes = plt.subplots(1, 4, figsize=(16, 4))
for ax, c in zip(axes, flags):
    sns.barplot(x=train_fe[c].astype(int), y=y, ax=ax)
    ax.set_xlabel(c)
    ax.set_ylabel('Средняя SalePrice')
    ax.set_title(c)
plt.tight_layout()
plt.show()

# ------------------------------------------------------------
# 6.6 ВИЗУАЛИЗАЦИЯ ТОП-ФИЧ ПРОТИВ ЦЕНЫ
# Топ-8 по MI (старые + новые): для числовых — scatter с регрессией,
# для категориальных — boxplot распределения цены по категориям.
# ------------------------------------------------------------
top8 = mi_scores2.head(8).index.tolist()
tmp = pd.concat([train_fe, pd.Series(y, name='SalePrice', index=train_fe.index)], axis=1)

fig, axes = plt.subplots(2, 4, figsize=(20, 10))
for ax, c in zip(axes.ravel(), top8):
    if tmp[c].dtype == 'object':
        order = tmp.groupby(c)['SalePrice'].median().sort_values(ascending=False).index
        sns.boxplot(data=tmp, x=c, y='SalePrice', order=order, ax=ax)
        ax.set_xticklabels(ax.get_xticklabels(), rotation=45, ha='right', fontsize=8)
        ax.set_title(f'{c}\nMI = {mi_scores2[c]:.3f}', fontsize=11)
    else:
        sns.regplot(data=tmp, x=c, y='SalePrice', scatter_kws={'s': 8, 'alpha': 0.4},
                    line_kws={'color': 'red'}, ax=ax)
        ax.set_title(f'{c}\nMI = {mi_scores2[c]:.3f}', fontsize=11)
plt.tight_layout()
plt.show()

# ------------------------------------------------------------
# 6.7 КОРРЕЛЯЦИЯ ФИЧ МЕЖДУ СОБОЙ + ОТБОР 10 РАЗНООБРАЗНЫХ
# Ищем признаки, которые сильны по связи с ценой, но слабо
# коррелируют ДРУГ С ДРУГОМ — чтобы не было дублирования.
# Pearson применим только к числовым, категории сюда не входят.
# ------------------------------------------------------------
num_fe_cols = [c for c in train_fe.columns if train_fe[c].dtype in ['int64', 'float64']]

# Сила признака = |корреляция с ценой|
strength = train_fe[num_fe_cols].corrwith(y_vis).abs().sort_values(ascending=False)

# Корреляция признаков между собой
corr_matrix = train_fe[num_fe_cols].corr()

# Жадный отбор: берём самые сильные, но не похожие на уже выбранные
selected = []
threshold = 0.7                      # макс. допустимая |корр| между отобранными
for c in strength.index:
    if all(abs(corr_matrix.loc[c, s]) < threshold for s in selected):
        selected.append(c)
    if len(selected) >= 10:
        break

print("\nТоп-10 сильных, но слабо коррелирующих друг с другом фич:")
for c in selected:
    mx = max([abs(corr_matrix.loc[c, s]) for s in selected if s != c]) if len(selected) > 1 else 0
    print(f"  {c:20s} |r с ценой| = {strength[c]:.3f}   макс |r| с остальными = {mx:.2f}")

# Таблица корреляций между 10 отобранными фичами
plt.figure(figsize=(11, 9))
sns.heatmap(train_fe[selected].corr(), annot=True, fmt=".2f",
            cmap="coolwarm", center=0, square=True)
plt.title('Корреляция между 10 отобранными фичами (должна быть низкой)')
plt.tight_layout()
plt.show()

# Полная таблица корреляций топ-20 сильных фич (видно дубликаты)
top20_strength = strength.head(20).index.tolist()
plt.figure(figsize=(14, 12))
sns.heatmap(train_fe[top20_strength].corr(), annot=True, fmt=".2f",
            cmap="coolwarm", center=0, square=True, linewidths=0.5)
plt.title('Корреляция топ-20 сильных фич между собой')
plt.tight_layout()
plt.show()

# Номинальные категории -> one-hot (dtype=float, чтобы всё было числом)
all_data = pd.get_dummies(all_data, dtype=float)
feature_names = all_data.columns.tolist()   # имена для графика важности

# Делим обратно на train и test
X       = all_data.iloc[:n_train].values.astype(float)   # признаки для обучения
X_test  = all_data.iloc[n_train:].values.astype(float)   # признаки для прогноза
print("Признаков:", X.shape[1])

# ------------------------------------------------------------
# 6.8 LASSO-РЕГРЕССИЯ: какие признаки модель обнуляет (можно выкинуть)
# L1-регуляризация зануляет коэффициенты неважных фич. Фичи с нулём
# не влияют на прогноз -> их можно удалить и сократить модель.
# ------------------------------------------------------------
from sklearn.linear_model import LassoCV
from sklearn.preprocessing import StandardScaler
from sklearn.pipeline import make_pipeline

lasso_pipe = make_pipeline(
    StandardScaler(),
    LassoCV(cv=5, random_state=42, n_alphas=200, max_iter=50000)
)
lasso_pipe.fit(X, y)
lasso_model = lasso_pipe.named_steps['lassocv']
coef = pd.Series(lasso_model.coef_, index=feature_names)

zero_coef    = coef[coef == 0]
nonzero_coef = coef[coef != 0]

print("\nLASSO alpha (подобран CV) =", round(lasso_model.alpha_, 4))
print(f"Всего признаков: {len(coef)}")
print(f"Занулено (можно выкинуть): {len(zero_coef)}")
print(f"Осталось значимых: {len(nonzero_coef)}")
print("\nПризнаки с нулевым коэффициентом:")
print(zero_coef.index.tolist())

# График: ненулевые коэффициенты (зелёные — положительные, красные — отрицательные)
plt.figure(figsize=(10, max(8, len(nonzero_coef) * 0.18)))
nc_sorted = nonzero_coef.sort_values()
colors = ['#2ecc71' if v > 0 else '#e74c3c' for v in nc_sorted.values]
plt.barh(nc_sorted.index, nc_sorted.values, color=colors)
plt.xlabel('Коэффициент LASSO')
plt.title(f'Ненулевые коэффициенты LASSO ({len(nonzero_coef)} фич)')
plt.tight_layout()
plt.show()

# ------------------------------------------------------------
# 6.8.1 ELASTIC NET — «LASSO, учитывающая корреляции»
# Lasso среди группы скоррелированных фич оставляет одну (произвольно).
# ElasticNet = Lasso + Ridge: L2-часть сжимает такие фичи вместе,
# поэтому скоррелированные признаки сохраняются группой.
# ------------------------------------------------------------
from sklearn.linear_model import ElasticNetCV

en_pipe = make_pipeline(
    StandardScaler(),
    ElasticNetCV(cv=5, l1_ratio=[0.1, 0.3, 0.5, 0.7, 0.9, 0.95, 1.0],
                 n_alphas=100, max_iter=50000, random_state=42)
)
en_pipe.fit(X, y)
en_model = en_pipe.named_steps['elasticnetcv']
en_coef = pd.Series(en_model.coef_, index=feature_names)

en_zero = en_coef[en_coef == 0]
print(f"\nELASTIC NET: l1_ratio = {en_model.l1_ratio_:.2f}, alpha = {en_model.alpha_:.4f}")
print(f"Занулено ElasticNet'ом: {len(en_zero)}  (Lasso занулил: {len(zero_coef)})")
print("ElasticNet оставляет скоррелированные фичи вместе, Lasso — по одной.")

# ------------------------------------------------------------
# 6.9 УДАЛЕНИЕ НЕЗНАЧИМЫХ ПРИЗНАКОВ (зануленных ElasticNet)
# Выкидываем фичи с нулевым коэффициентом из матрицы признаков.
# Дальше все модели обучаются уже на сокращённом наборе.
# ------------------------------------------------------------
en_nonzero = en_coef[en_coef != 0]
keep_cols  = en_nonzero.index.tolist()
keep_idx   = [feature_names.index(c) for c in keep_cols]

X             = X[:, keep_idx]
X_test        = X_test[:, keep_idx]
feature_names = keep_cols

print(f"\nПосле удаления незначимых фич (ElasticNet) осталось: {X.shape[1]} (было {len(en_coef)})")

# ------------------------------------------------------------
# 7. ЕДИНЫЙ CV-ХЕЛПЕР (умеет лог-трансформацию цели)
# ------------------------------------------------------------
from sklearn.linear_model import Ridge, QuantileRegressor
from sklearn.preprocessing import StandardScaler
from sklearn.pipeline import make_pipeline
from sklearn.ensemble import RandomForestRegressor, HistGradientBoostingRegressor
from catboost import CatBoostRegressor

NFOLDS = 5          # 10 — стабильнее, но дольше
kf = KFold(n_splits=NFOLDS, shuffle=True, random_state=42)
ylog = np.log1p(y)                     # лог-цель для части моделей
inv  = np.expm1                        # обратное преобразование

def cv(factory, ytarget, inverse=None, es=False, name=""):
    oof = np.zeros(len(ytarget))
    pred = np.zeros(X_test.shape[0])
    for i, (tr, va) in enumerate(kf.split(X), 1):
        m = factory()
        if es:
            m.fit(X[tr], ytarget[tr], eval_set=[(X[va], ytarget[va])],
                  callbacks=[lgb.early_stopping(100, verbose=False)])
        else:
            m.fit(X[tr], ytarget[tr])
        o, p = m.predict(X[va]), m.predict(X_test)
        if inverse is not None:
            o, p = inverse(o), inverse(p)
        oof[va] = o
        pred += p / NFOLDS
        print(f"{name}: фолд {i}/{NFOLDS}")
    return oof, pred

# ------------------------------------------------------------
# 8. МОДЕЛИ: СЫРАЯ ЦЕЛЬ (L1) + ЛОГ-ЦЕЛЬ + ЛИНЕЙНАЯ + ЛЕС
# ------------------------------------------------------------
models = {}

models['lgb'] = cv(lambda: lgb.LGBMRegressor(objective='regression_l1',
    n_estimators=2000, learning_rate=0.03, num_leaves=31, subsample=0.8,
    colsample_bytree=0.8, random_state=42, verbose=-1), y, es=True, name='lgb')

models['xgb'] = cv(lambda: XGBRegressor(objective='reg:absoluteerror',
    n_estimators=1200, learning_rate=0.03, max_depth=6, subsample=0.8,
    colsample_bytree=0.8, tree_method='hist', verbosity=0, random_state=42),
    y, name='xgb')

models['hgb'] = cv(lambda: HistGradientBoostingRegressor(loss='absolute_error',
    max_iter=800, learning_rate=0.05, max_leaf_nodes=31, random_state=42),
    y, name='hgb')

models['cat'] = cv(lambda: CatBoostRegressor(loss_function='MAE', iterations=2000,
    learning_rate=0.05, depth=6, verbose=0, random_seed=42), y, name='cat')

# --- лог-модели (L2 на log1p, возврат через expm1) ---
models['lgb_log'] = cv(lambda: lgb.LGBMRegressor(n_estimators=2000, learning_rate=0.03,
    num_leaves=31, subsample=0.8, colsample_bytree=0.8, random_state=42, verbose=-1),
    ylog, inverse=inv, es=True, name='lgb_log')

models['xgb_log'] = cv(lambda: XGBRegressor(n_estimators=1200, learning_rate=0.03,
    max_depth=6, subsample=0.8, colsample_bytree=0.8, tree_method='hist',
    verbosity=0, random_state=42), ylog, inverse=inv, name='xgb_log')

models['cat_log'] = cv(lambda: CatBoostRegressor(iterations=2000, learning_rate=0.05,
    depth=6, verbose=0, random_seed=42), ylog, inverse=inv, name='cat_log')

# --- разнообразие: линейная модель и лес ---
models['ridge'] = cv(lambda: make_pipeline(StandardScaler(), Ridge(alpha=10.0)),
    y, name='ridge')

models['rf'] = cv(lambda: RandomForestRegressor(n_estimators=500, min_samples_leaf=2,
    random_state=42, n_jobs=-1), y, name='rf')

# ------------------------------------------------------------
# 9. СТЕКИНГ: QuantileRegressor(0.5) минимизирует именно MAE
# ------------------------------------------------------------
names = list(models)
OOF  = np.vstack([models[n][0] for n in names]).T
PRED = np.vstack([models[n][1] for n in names]).T

for n in names:
    print(f"{n:8s} MAE:", round(mean_absolute_error(y, models[n][0]), 2))

stack = QuantileRegressor(quantile=0.5, alpha=0.0, solver='highs')
stack.fit(OOF, y)
print("СТЕК  MAE:", round(mean_absolute_error(y, stack.predict(OOF)), 2))
print("коэффициенты:", np.round(stack.coef_, 4))

# ------------------------------------------------------------
# 9.5 ВАЖНОСТЬ ПРИЗНАКОВ (LightGBM) — какие фичи решают больше всего
# ------------------------------------------------------------
imp_model = lgb.LGBMRegressor(objective='regression_l1', n_estimators=500,
                              learning_rate=0.05, random_state=42, verbose=-1)
imp_model.fit(X, y)
imp = pd.Series(imp_model.feature_importances_, index=feature_names).sort_values(ascending=False)

plt.figure(figsize=(10, 10))
imp.head(30)[::-1].plot(kind='barh', color='#3498db')
plt.title('Топ-30 признаков по важности (LightGBM)')
plt.xlabel('Feature importance')
plt.tight_layout()
plt.show()

# ------------------------------------------------------------
# 10. САБМИТ
# ------------------------------------------------------------
final = np.clip(stack.predict(PRED), y.min(), y.max())
pd.DataFrame({'Id': test_ids, 'SalePrice': final}).to_csv('submission.csv', index=False)
print("submission.csv сохранён")
