import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { CustomClearanceEditComponent } from './custom-clearance-edit.component';

describe('CustomClearanceEditComponent', () => {
  let component: CustomClearanceEditComponent;
  let fixture: ComponentFixture<CustomClearanceEditComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ CustomClearanceEditComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CustomClearanceEditComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
