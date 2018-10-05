import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { InlandTruckingComponent } from './inland-trucking.component';

describe('InlandTruckingComponent', () => {
  let component: InlandTruckingComponent;
  let fixture: ComponentFixture<InlandTruckingComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ InlandTruckingComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(InlandTruckingComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
