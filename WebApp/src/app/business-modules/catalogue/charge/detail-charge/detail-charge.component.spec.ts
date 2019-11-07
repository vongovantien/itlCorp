import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { DetailChargeComponent } from './detail-charge.component';

describe('DetailChargeComponent', () => {
  let component: DetailChargeComponent;
  let fixture: ComponentFixture<DetailChargeComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ DetailChargeComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(DetailChargeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
