import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ChargeImportComponent } from './charge-import.component';

describe('ChargeImportComponent', () => {
  let component: ChargeImportComponent;
  let fixture: ComponentFixture<ChargeImportComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ChargeImportComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ChargeImportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
