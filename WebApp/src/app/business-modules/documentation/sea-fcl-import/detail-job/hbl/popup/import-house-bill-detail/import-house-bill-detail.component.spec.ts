import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ImportHouseBillDetailComponent } from './import-house-bill-detail.component';

describe('ImportHouseBillDetailComponent', () => {
  let component: ImportHouseBillDetailComponent;
  let fixture: ComponentFixture<ImportHouseBillDetailComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ImportHouseBillDetailComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ImportHouseBillDetailComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
