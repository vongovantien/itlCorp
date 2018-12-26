import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { StageImportComponent } from './stage-import.component';

describe('StageImportComponent', () => {
  let component: StageImportComponent;
  let fixture: ComponentFixture<StageImportComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ StageImportComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(StageImportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
