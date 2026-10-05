# -*- coding: utf-8 -*-
"""
Анализ признаков набора Ames Housing (House Prices) методом Mutual Information.

Что делает скрипт:
  1) Загружает train.csv, готовит данные (заполнение пропусков, кодирование).
  2) Считает Mutual Information (взаимную информацию) каждой фичи с SalePrice.
  3) Строит график MI по всем фичам + отдельно топ-20.
  4) Выделяет наиболее значимые фичи и обосновывает выбор.
  5) Выбирает топ-3 самые интересные фичи и рисует их связь с ценой.
Все графики сохраняются в текущую папку в формате PNG.
"""

import sys

# Чтобы русский текст в консоли Windows не падал с ошибкой кодировки
try:
    sys.stdout.reconfigure(encoding="utf-8")
except Exception:
    pass

import numpy as np
import pandas as pd
import matplotlib.pyplot as plt
import seaborn as sns
from sklearn.feature_selection import mutual_info_regression

sns.set_theme(style="whitegrid", palette="viridis")

TRAIN_PATH = "train.csv"
TEST_PATH = "test.csv"

# ----------------------------------------------------------------------
# 1. Загрузка данных
# ----------------------------------------------------------------------
train = pd.read_csv(TRAIN_PATH)
test = pd.read_csv(TEST_PATH)

print("=" * 70)
print("Размерности: train =", train.shape, "| test =", test.shape)
print("=" * 70)

# Целевая переменная
y = train["SalePrice"]

# Объединяем train и test для единой предобработки (без SalePrice)
all_data = pd.concat([train.drop(columns=["SalePrice"]), test], axis=0).reset_index(drop=True)
train_features = all_data.iloc[: len(train)].reset_index(drop=True)

# Отделяем числовые и категориальные колонки
numeric_cols = [c for c in train_features.columns if train_features[c].dtype in ["int64", "float64"]]
categorical_cols = [c for c in train_features.columns if train_features[c].dtype == "object"]

print(f"Числовых признаков: {len(numeric_cols)}")
print(f"Категориальных признаков: {len(categorical_cols)}")

# ----------------------------------------------------------------------
# 2. Предобработка: пропуски + кодирование категорий
# ----------------------------------------------------------------------
# Числовые пропуски заполняем медианой (устойчиво к выбросам)
for col in numeric_cols:
    if train_features[col].isna().any():
        train_features[col] = train_features[col].fillna(train_features[col].median())

# Категориальные пропуски помечаем отдельной категорией "Missing"
for col in categorical_cols:
    train_features[col] = train_features[col].fillna("Missing")

# Кодируем категории целыми кодами (для MI категория должна быть дискретной)
X = train_features.copy()
for col in categorical_cols:
    X[col] = X[col].astype("category").cat.codes

# Маска: какие признаки считать дискретными при расчёте MI
discrete_features = [col in categorical_cols for col in X.columns]

# ----------------------------------------------------------------------
# 3. Mutual Information по всем фичам
# ----------------------------------------------------------------------
print("\nВычисление Mutual Information для всех фич...")

mi_scores = mutual_info_regression(
    X, y,
    discrete_features=discrete_features,
    random_state=42,
    n_neighbors=5,
)
mi_series = (
    pd.Series(mi_scores, index=X.columns, name="Mutual Information")
    .sort_values(ascending=False)
)

print("\nТоп-20 фич по Mutual Information:")
print(mi_series.head(20).round(4).to_string())

# ----------------------------------------------------------------------
# 4. График: MI по всем фичам
# ----------------------------------------------------------------------
plt.figure(figsize=(12, max(8, len(mi_series) * 0.28)))
colors = ["#c0392b" if v >= mi_series.iloc[2] else "#3498db" for v in mi_series.values]
plt.barh(mi_series.index[::-1], mi_series.values[::-1], color=colors[::-1])
plt.xlabel("Mutual Information (биты)", fontsize=12)
plt.title("Mutual Information всех признаков с SalePrice", fontsize=14)
plt.tight_layout()
plt.savefig("mi_all_features.png", dpi=150)
plt.close()
print("\nСохранено: mi_all_features.png")

# ----------------------------------------------------------------------
# 5. Топ-20 признаков крупным планом
# ----------------------------------------------------------------------
top20 = mi_series.head(20)
plt.figure(figsize=(10, 8))
bars = plt.barh(top20.index[::-1], top20.values[::-1])
bars[-1].set_color("#c0392b")
bars[-2].set_color("#c0392b")
bars[-3].set_color("#c0392b")
plt.xlabel("Mutual Information (биты)", fontsize=12)
plt.title("Топ-20 признаков по Mutual Information", fontsize=14)
for i, v in enumerate(top20.values[::-1]):
    plt.text(v + 0.002, i, f"{v:.3f}", va="center", fontsize=9)
plt.tight_layout()
plt.savefig("mi_top20.png", dpi=150)
plt.close()
print("Сохранено: mi_top20.png")

# ----------------------------------------------------------------------
# 6. Наиболее значимые фичи: порог значимости
# ----------------------------------------------------------------------
# Значимыми считаем фичи, у которых MI > среднего MI по всем признакам.
# Это простой, но наглядный критерий "выше среднего уровня".
threshold = mi_series.mean()
significant = mi_series[mi_series > threshold]

print("\n" + "=" * 70)
print(f"Порог значимости (средний MI) = {threshold:.4f}")
print(f"Наиболее значимых фич: {len(significant)}")
print("=" * 70)
print(significant.round(4).to_string())

# ----------------------------------------------------------------------
# 7. Топ-3 самые интересные фичи
# ----------------------------------------------------------------------
top3 = mi_series.head(3)
print("\nТоп-3 самые интересные фичи с высоким MI:")
for name, value in top3.items():
    col_type = "категориальная" if name in categorical_cols else "числовая"
    print(f"  {name}: MI = {value:.4f} ({col_type})")

# Построение графиков связи топ-3 фич с ценой
fig, axes = plt.subplots(1, 3, figsize=(18, 5))
for ax, (name, _) in zip(axes, top3.items()):
    series = train[name]
    if name in categorical_cols:
        # Для категории — boxplot распределения цены по категориям
        order = (
            train.groupby(name)["SalePrice"].median().sort_values(ascending=False).index
        )
        sns.boxplot(x=series, y=y, order=order, ax=ax)
        ax.set_xticklabels(ax.get_xticklabels(), rotation=45, ha="right", fontsize=8)
        ax.set_ylabel("SalePrice")
        ax.set_title(f"{name}\nMI = {top3[name]:.3f}", fontsize=12)
    else:
        # Для числа — scatter с линией регрессии
        sns.regplot(x=series, y=y, scatter_kws={"s": 10, "alpha": 0.4}, ax=ax, line_kws={"color": "red"})
        corr = np.corrcoef(series, y)[0, 1]
        ax.set_xlabel(name)
        ax.set_ylabel("SalePrice")
        ax.set_title(f"{name}\nMI = {top3[name]:.3f}, corr = {corr:.2f}", fontsize=12)
plt.tight_layout()
plt.savefig("mi_top3_relationships.png", dpi=150)
plt.close()
print("Сохранено: mi_top3_relationships.png")

# ----------------------------------------------------------------------
# 8. Корреляционная матрица топ-15 признаков (контекст)
# ----------------------------------------------------------------------
top15_cols = mi_series.head(15).index.tolist()
corr = X[top15_cols].corr()
plt.figure(figsize=(12, 10))
sns.heatmap(corr, annot=True, fmt=".2f", cmap="coolwarm", center=0, square=True)
plt.title("Корреляция между топ-15 признаками по MI", fontsize=14)
plt.tight_layout()
plt.savefig("mi_top15_corr_heatmap.png", dpi=150)
plt.close()
print("Сохранено: mi_top15_corr_heatmap.png")

# ----------------------------------------------------------------------
# 9. Итоговая сводка в файл
# ----------------------------------------------------------------------
with open("mi_scores.csv", "w", encoding="utf-8") as f:
    mi_series.to_csv(f, header=True)

print("\n" + "=" * 70)
print("ГОТОВО. Результаты:")
print("  - mi_scores.csv                — MI всех фич (таблица)")
print("  - mi_all_features.png          — график MI всех фич")
print("  - mi_top20.png                 — топ-20 фич")
print("  - mi_top3_relationships.png    — связь топ-3 фич с ценой")
print("  - mi_top15_corr_heatmap.png    — корреляция топ-15 фич")
print("=" * 70)
