import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { PortIndexImportComponent } from './port-index-import.component';

describe('PortIndexImportComponent', () => {
  let component: PortIndexImportComponent;
  let fixture: ComponentFixture<PortIndexImportComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ PortIndexImportComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(PortIndexImportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
