import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { FormAddHouseBillComponent } from './form-add-house-bill.component';

describe('FormAddHouseBillComponent', () => {
  let component: FormAddHouseBillComponent;
  let fixture: ComponentFixture<FormAddHouseBillComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ FormAddHouseBillComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FormAddHouseBillComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
