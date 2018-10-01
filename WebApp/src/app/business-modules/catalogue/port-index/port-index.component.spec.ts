import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { PortIndexComponent } from './port-index.component';

describe('PortIndexComponent', () => {
  let component: PortIndexComponent;
  let fixture: ComponentFixture<PortIndexComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ PortIndexComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(PortIndexComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
