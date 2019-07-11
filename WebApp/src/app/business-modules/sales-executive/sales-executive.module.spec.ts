import { SalesExecutiveModule } from './sales-executive.module';

describe('SalesExecutiveModule', () => {
  let salesExecutiveModule: SalesExecutiveModule;

  beforeEach(() => {
    salesExecutiveModule = new SalesExecutiveModule();
  });

  it('should create an instance', () => {
    expect(salesExecutiveModule).toBeTruthy();
  });
});
