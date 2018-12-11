import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { WarehouseImportComponent } from './warehouse-import.component';

describe('WarehouseImportComponent', () => {
  let component: WarehouseImportComponent;
  let fixture: ComponentFixture<WarehouseImportComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ WarehouseImportComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(WarehouseImportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
