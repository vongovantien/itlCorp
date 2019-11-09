import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ContainerImportComponent } from './container-import.component';

describe('ContainerImportComponent', () => {
  let component: ContainerImportComponent;
  let fixture: ComponentFixture<ContainerImportComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ContainerImportComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ContainerImportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
