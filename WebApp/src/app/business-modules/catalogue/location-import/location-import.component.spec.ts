import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { LocationImportComponent } from './location-import.component';

describe('LocationImportComponent', () => {
  let component: LocationImportComponent;
  let fixture: ComponentFixture<LocationImportComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ LocationImportComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(LocationImportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
