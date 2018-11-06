import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ModifiedModalComponent } from './modified-modal.component';

describe('ModifiedModalComponent', () => {
  let component: ModifiedModalComponent;
  let fixture: ComponentFixture<ModifiedModalComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ModifiedModalComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ModifiedModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
