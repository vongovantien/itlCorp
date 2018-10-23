import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { PagerSetting } from '../../models/layout/pager-setting.model';
import { PagingService } from './paging-service';

@Component({
  selector: 'app-pagination',
  templateUrl: './pagination.component.html',
  styleUrls: ['./pagination.component.scss']
})
export class PaginationComponent implements OnInit {
   @Input() config: PagerSetting = {};
   @Output() pagerObject = new EventEmitter<any[]>();
   count = 0;
  
   selectPageSize(){
    this.pager.pageSize = this.config.pageSize;
    this.setPage(1);
    this.pagerObject.emit(this.pager);
    
  }

  constructor(private pagerService: PagingService) { }

  // pager object
  pager: any = {};
  ngOnInit() {   
    console.log(this.config);
    this.setPage(this.config.currentPage);
  }

  sendBackData() {
    this.count += 1;
    console.log(this.count)
  }

  setPage(page: number) {
    if (page < 1 || page > this.pager.totalPages) {
      return;
    }
    this.pager = this.pagerService.getPager(this.config.totalItems, page, this.config.pageSize,this.config.totalPageBtn);
    this.pagerObject.emit(this.pager);
  }
}
