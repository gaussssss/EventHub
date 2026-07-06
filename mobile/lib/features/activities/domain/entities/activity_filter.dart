import 'package:flutter/material.dart';

/// Sentinelle interne permettant à [ActivityFilter.copyWith] de distinguer
/// « champ non fourni » de « champ explicitement mis à null ».
const Object _unset = Object();

/// État de filtrage du catalogue, immuable. Poussé au backend via query params
/// (`category`, `availableOnly`, `from`/`to`). Par défaut, les activités passées
/// sont masquées ; [includePast] permet de voir les anciennes.
@immutable
class ActivityFilter {
  final String? categorySlug;
  final bool availableOnly;
  final DateTimeRange? dateRange;
  final bool includePast;

  const ActivityFilter({
    this.categorySlug,
    this.availableOnly = false,
    this.dateRange,
    this.includePast = false,
  });

  /// Vrai dès qu'au moins un critère (hors défaut) est actif.
  bool get isActive =>
      categorySlug != null || availableOnly || dateRange != null || includePast;

  /// Nombre de critères actifs (sert au badge du bouton filtre).
  int get activeCount =>
      (categorySlug != null ? 1 : 0) +
      (availableOnly ? 1 : 0) +
      (dateRange != null ? 1 : 0) +
      (includePast ? 1 : 0);

  ActivityFilter copyWith({
    Object? categorySlug = _unset,
    bool? availableOnly,
    Object? dateRange = _unset,
    bool? includePast,
  }) {
    return ActivityFilter(
      categorySlug: identical(categorySlug, _unset)
          ? this.categorySlug
          : categorySlug as String?,
      availableOnly: availableOnly ?? this.availableOnly,
      dateRange: identical(dateRange, _unset)
          ? this.dateRange
          : dateRange as DateTimeRange?,
      includePast: includePast ?? this.includePast,
    );
  }

  @override
  bool operator ==(Object other) =>
      other is ActivityFilter &&
      other.categorySlug == categorySlug &&
      other.availableOnly == availableOnly &&
      other.dateRange == dateRange &&
      other.includePast == includePast;

  @override
  int get hashCode =>
      Object.hash(categorySlug, availableOnly, dateRange, includePast);
}
