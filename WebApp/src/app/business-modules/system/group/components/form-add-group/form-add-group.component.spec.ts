import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { FormAddGroupComponent } from './form-add-group.component';

describe('FormAddGroupComponent', () => {
  let component: FormAddGroupComponent;
  let fixture: ComponentFixture<FormAddGroupComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ FormAddGroupComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FormAddGroupComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
