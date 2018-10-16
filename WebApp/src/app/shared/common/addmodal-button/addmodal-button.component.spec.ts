import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AddmodalButtonComponent } from './addmodal-button.component';

describe('AddmodalButtonComponent', () => {
  let component: AddmodalButtonComponent;
  let fixture: ComponentFixture<AddmodalButtonComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AddmodalButtonComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AddmodalButtonComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
