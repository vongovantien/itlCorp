import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { CustomClearanceAddnewComponent } from './custom-clearance-addnew.component';

describe('CustomClearanceAddnewComponent', () => {
  let component: CustomClearanceAddnewComponent;
  let fixture: ComponentFixture<CustomClearanceAddnewComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ CustomClearanceAddnewComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CustomClearanceAddnewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
