import { DesignModulesModule } from './design-modules.module';

describe('DesignModulesModule', () => {
  let designModulesModule: DesignModulesModule;

  beforeEach(() => {
    designModulesModule = new DesignModulesModule();
  });

  it('should create an instance', () => {
    expect(designModulesModule).toBeTruthy();
  });
});
