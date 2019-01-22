import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { HousebillAddnewComponent } from './housebill-addnew.component';

describe('HousebillAddnewComponent', () => {
  let component: HousebillAddnewComponent;
  let fixture: ComponentFixture<HousebillAddnewComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ HousebillAddnewComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(HousebillAddnewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
