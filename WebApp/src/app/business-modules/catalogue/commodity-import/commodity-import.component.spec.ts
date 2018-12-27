import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { CommodityImportComponent } from './commodity-import.component';

describe('CommodityImportComponent', () => {
  let component: CommodityImportComponent;
  let fixture: ComponentFixture<CommodityImportComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ CommodityImportComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CommodityImportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
