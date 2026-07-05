import { ModerationStates } from './moderationStates';
import { ReportDto } from '../../../models/reportDto';

function report(id: string): ReportDto {
  return {
    id,
    targetType: 'post',
    targetId: 'p',
    reason: 'x',
    status: 'open',
    reporterName: 'R',
    createdAt: '2026-01-01T00:00:00Z',
  };
}

describe('ModerationStates', () => {
  let states: ModerationStates;

  beforeEach(() => (states = new ModerationStates()));

  it('retire un signalement par id', () => {
    states.setReports([report('a'), report('b')]);
    states.removeById('a');
    expect(states.reports().map((r) => r.id)).toEqual(['b']);
  });

  it('réinitialise la file', () => {
    states.setReports([report('a')]);
    states.reset();
    expect(states.reports()).toEqual([]);
  });
});
