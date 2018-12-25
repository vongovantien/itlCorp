import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { MasterBillComponent } from './master-bill.component';

describe('MasterBillComponent', () => {
  let component: MasterBillComponent;
  let fixture: ComponentFixture<MasterBillComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ MasterBillComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(MasterBillComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
