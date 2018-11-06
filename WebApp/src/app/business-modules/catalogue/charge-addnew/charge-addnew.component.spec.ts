import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ChargeAddnewComponent } from './charge-addnew.component';

describe('ChargeAddnewComponent', () => {
  let component: ChargeAddnewComponent;
  let fixture: ComponentFixture<ChargeAddnewComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ChargeAddnewComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ChargeAddnewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
