import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { HousebillImportDetailComponent } from './housebill-import-detail.component';

describe('HousebillImportDetailComponent', () => {
  let component: HousebillImportDetailComponent;
  let fixture: ComponentFixture<HousebillImportDetailComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ HousebillImportDetailComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(HousebillImportDetailComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
