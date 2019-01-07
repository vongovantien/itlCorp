import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { PartnerDataImportComponent } from './partner-data-import.component';

describe('PartnerDataImportComponent', () => {
  let component: PartnerDataImportComponent;
  let fixture: ComponentFixture<PartnerDataImportComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ PartnerDataImportComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(PartnerDataImportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
