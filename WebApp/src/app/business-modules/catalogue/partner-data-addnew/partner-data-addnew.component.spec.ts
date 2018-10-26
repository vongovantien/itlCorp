import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { PartnerDataAddnewComponent } from './partner-data-addnew.component';

describe('PartnerDataAddnewComponent', () => {
  let component: PartnerDataAddnewComponent;
  let fixture: ComponentFixture<PartnerDataAddnewComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ PartnerDataAddnewComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(PartnerDataAddnewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
