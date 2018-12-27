import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { CommodityGroupImportComponent } from './commodity-group-import.component';

describe('CommodityGroupImportComponent', () => {
  let component: CommodityGroupImportComponent;
  let fixture: ComponentFixture<CommodityGroupImportComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ CommodityGroupImportComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CommodityGroupImportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
