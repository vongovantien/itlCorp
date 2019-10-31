import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { FormCreateHouseBillComponent } from './form-create-house-bill.component';

describe('FormCreateHouseBillComponent', () => {
  let component: FormCreateHouseBillComponent;
  let fixture: ComponentFixture<FormCreateHouseBillComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ FormCreateHouseBillComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FormCreateHouseBillComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
