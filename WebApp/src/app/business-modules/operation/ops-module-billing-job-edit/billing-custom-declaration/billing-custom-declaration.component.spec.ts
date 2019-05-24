import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { BillingCustomDeclarationComponent } from './billing-custom-declaration.component';

describe('BillingCustomDeclarationComponent', () => {
  let component: BillingCustomDeclarationComponent;
  let fixture: ComponentFixture<BillingCustomDeclarationComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ BillingCustomDeclarationComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(BillingCustomDeclarationComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
