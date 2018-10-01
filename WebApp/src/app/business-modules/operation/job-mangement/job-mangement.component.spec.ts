import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { JobMangementComponent } from './job-mangement.component';

describe('JobMangementComponent', () => {
  let component: JobMangementComponent;
  let fixture: ComponentFixture<JobMangementComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ JobMangementComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(JobMangementComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
