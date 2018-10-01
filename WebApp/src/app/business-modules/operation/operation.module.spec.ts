import { OperationModule } from './operation.module';

describe('OperationModule', () => {
  let operationModule: OperationModule;

  beforeEach(() => {
    operationModule = new OperationModule();
  });

  it('should create an instance', () => {
    expect(operationModule).toBeTruthy();
  });
});
