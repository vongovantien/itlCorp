import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { HousebillListComponent } from './housebill-list.component';

describe('HousebillListComponent', () => {
  let component: HousebillListComponent;
  let fixture: ComponentFixture<HousebillListComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ HousebillListComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(HousebillListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
