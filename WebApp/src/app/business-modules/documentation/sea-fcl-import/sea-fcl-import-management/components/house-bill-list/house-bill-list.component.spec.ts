import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { HouseBillListComponent } from './house-bill-list.component';

describe('HouseBillListComponent', () => {
  let component: HouseBillListComponent;
  let fixture: ComponentFixture<HouseBillListComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ HouseBillListComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(HouseBillListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
