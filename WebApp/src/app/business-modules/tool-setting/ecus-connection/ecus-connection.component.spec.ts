import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { EcusConnectionComponent } from './ecus-connection.component';

describe('EcusConnectionComponent', () => {
  let component: EcusConnectionComponent;
  let fixture: ComponentFixture<EcusConnectionComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ EcusConnectionComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(EcusConnectionComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
