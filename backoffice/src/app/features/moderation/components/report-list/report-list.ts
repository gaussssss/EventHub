import { DatePipe } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { ModalStates } from '../../../../shared/services/infrastructure/states/modalStates';
import { HideTarget } from '../../services/application/hideTarget';
import { LoadReports } from '../../services/application/loadReports';
import { ModerationStates } from '../../services/infrastructure/states/moderationStates';
import { ReportDto } from '../../models/reportDto';
import { HideConfirmModal } from '../modals/hide-confirm/hide-confirm';

/** Écran « Modération » : file des signalements + confirmation/masquage. */
@Component({
  selector: 'app-report-list',
  standalone: true,
  imports: [DatePipe, HideConfirmModal],
  templateUrl: './report-list.html',
})
export class ReportList implements OnInit {
  readonly states = inject(ModerationStates);
  readonly load = inject(LoadReports);
  readonly hide = inject(HideTarget);
  readonly modals = inject(ModalStates);

  ngOnInit(): void {
    this.load.handler();
  }

  openHide(report: ReportDto): void {
    this.modals.open('moderation-hide', {
      reportId: report.id,
      targetType: report.targetType,
      targetId: report.targetId,
      authorName: report.targetAuthorName ?? '',
      preview: report.targetPreview ?? '',
      imageUrl: report.targetImageUrl ?? '',
      reason: report.reason,
    });
  }

  isComment(type: string): boolean {
    return type.toLowerCase() === 'comment';
  }

  targetIcon(type: string): string {
    return this.isComment(type)
      ? 'icon-[fluent--comment-24-regular]'
      : 'icon-[fluent--document-24-regular]';
  }

  targetLabel(type: string): string {
    return this.isComment(type) ? 'Commentaire' : 'Publication';
  }

  shortId(id: string): string {
    return id.slice(0, 8);
  }
}
