import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { UpdateProvinceComponent } from './update-province.component';

describe('UpdateProvinceComponent', () => {
  let component: UpdateProvinceComponent;
  let fixture: ComponentFixture<UpdateProvinceComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ UpdateProvinceComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(UpdateProvinceComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
