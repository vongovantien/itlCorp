import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { CustomClearanceImportComponent } from './custom-clearance-import.component';

describe('CustomClearanceImportComponent', () => {
  let component: CustomClearanceImportComponent;
  let fixture: ComponentFixture<CustomClearanceImportComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ CustomClearanceImportComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CustomClearanceImportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
