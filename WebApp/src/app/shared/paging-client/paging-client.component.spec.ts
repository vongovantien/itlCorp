import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { PagingClientComponent } from './paging-client.component';

describe('PagingClientComponent', () => {
  let component: PagingClientComponent;
  let fixture: ComponentFixture<PagingClientComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ PagingClientComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(PagingClientComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
