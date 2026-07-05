import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { ToastStates } from '../../../../shared/services/infrastructure/states/toastStates';
import { ReportDto } from '../../models/reportDto';
import { ModerationService } from '../infrastructure/repository/moderation';
import { ModerationStates } from '../infrastructure/states/moderationStates';
import { HideTarget } from './hideTarget';

function report(id: string, targetType: string, targetId: string): ReportDto {
  return {
    id,
    targetType,
    targetId,
    reason: 'x',
    status: 'open',
    reporterName: 'R',
    createdAt: '2026-01-01T00:00:00Z',
  };
}

function setup() {
  const hidePost = jasmine.createSpy('hidePost').and.returnValue(of(void 0));
  const hideComment = jasmine.createSpy('hideComment').and.returnValue(of(void 0));
  TestBed.configureTestingModule({
    providers: [{ provide: ModerationService, useValue: { hidePost, hideComment } }],
  });
  return {
    hide: TestBed.inject(HideTarget),
    states: TestBed.inject(ModerationStates),
    toasts: TestBed.inject(ToastStates),
    hidePost,
    hideComment,
  };
}

describe('HideTarget', () => {
  it('masque une publication et retire le signalement de la file', () => {
    const { hide, states, hidePost, hideComment } = setup();
    states.setReports([report('r1', 'post', 'p1')]);

    hide.handler('r1', 'post', 'p1');

    expect(hidePost).toHaveBeenCalledWith('p1');
    expect(hideComment).not.toHaveBeenCalled();
    expect(states.reports().length).toBe(0);
    expect(hide.hidingId()).toBeNull();
  });

  it('route vers hideComment pour un commentaire (insensible à la casse)', () => {
    const { hide, states, hidePost, hideComment } = setup();
    states.setReports([report('r2', 'Comment', 'c1')]);

    hide.handler('r2', 'Comment', 'c1');

    expect(hideComment).toHaveBeenCalledWith('c1');
    expect(hidePost).not.toHaveBeenCalled();
    expect(states.reports().length).toBe(0);
  });
});
