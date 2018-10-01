import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SeaConsolImportComponent } from './sea-consol-import.component';

describe('SeaConsolImportComponent', () => {
  let component: SeaConsolImportComponent;
  let fixture: ComponentFixture<SeaConsolImportComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ SeaConsolImportComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SeaConsolImportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
