import 'package:flutter/material.dart';
import 'activity.dart';

/// Sentinelle interne permettant à [ActivityFilter.copyWith] de distinguer
/// « champ non fourni » de « champ explicitement mis à null ».
const Object _unset = Object();

/// État de filtrage du catalogue, immuable.
@immutable
class ActivityFilter {
  final ActivityCategory? category;
  final bool availableOnly;
  final DateTimeRange? dateRange;

  const ActivityFilter({
    this.category,
    this.availableOnly = false,
    this.dateRange,
  });

  /// Vrai dès qu'au moins un critère est actif.
  bool get isActive =>
      category != null || availableOnly || dateRange != null;

  /// Nombre de critères actifs (sert au badge du bouton filtre).
  int get activeCount =>
      (category != null ? 1 : 0) +
      (availableOnly ? 1 : 0) +
      (dateRange != null ? 1 : 0);

  ActivityFilter copyWith({
    Object? category = _unset,
    bool? availableOnly,
    Object? dateRange = _unset,
  }) {
    return ActivityFilter(
      category: identical(category, _unset)
          ? this.category
          : category as ActivityCategory?,
      availableOnly: availableOnly ?? this.availableOnly,
      dateRange: identical(dateRange, _unset)
          ? this.dateRange
          : dateRange as DateTimeRange?,
    );
  }

  @override
  bool operator ==(Object other) =>
      other is ActivityFilter &&
      other.category == category &&
      other.availableOnly == availableOnly &&
      other.dateRange == dateRange;

  @override
  int get hashCode => Object.hash(category, availableOnly, dateRange);
}
