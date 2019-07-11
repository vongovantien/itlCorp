import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { CustomClearanceComponent } from './custom-clearance.component';

describe('CustomClearanceComponent', () => {
  let component: CustomClearanceComponent;
  let fixture: ComponentFixture<CustomClearanceComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ CustomClearanceComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CustomClearanceComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
