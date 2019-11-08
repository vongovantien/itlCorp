import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { FormSearchHouseBillComponent } from './form-search-house-bill.component';

describe('FormSearchHouseBillComponent', () => {
  let component: FormSearchHouseBillComponent;
  let fixture: ComponentFixture<FormSearchHouseBillComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ FormSearchHouseBillComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FormSearchHouseBillComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
