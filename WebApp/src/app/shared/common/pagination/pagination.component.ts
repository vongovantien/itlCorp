import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { PagerService } from '../../services/pager.service';
import { PagerSetting } from '../../models/layout/pager-setting.model';
import { PagingService } from './paging-service';

@Component({
  selector: 'app-pagination',
  templateUrl: './pagination.component.html',
  styleUrls: ['./pagination.component.scss']
})
export class PaginationComponent implements OnInit {
  //@Input() totalItems: number = 1;
  //@Input() numberPageDisplay: number = 3;
  // @Input() config: PagerSetting = {};
   @Output() pageChange: EventEmitter<number> = new EventEmitter<number>();

  // pager: any = {};

  // constructor(private pagerService: PagerService) { }

  // ngOnInit() {
  //   console.log(this.config);
  //   this.getPages(this.config.currentPage, this.config.pageSize, this.config.totalItems);
  // }

  // ngOnChanges(): void {
  //   this.getPages(this.config.currentPage, this.config.pageSize, this.config.totalItems);
  // }
  
  // getTotalPages(itemsPerPage: number, totalItems: number): number {
  //   return Math.ceil(Math.max(totalItems, 1) / Math.max(itemsPerPage, 1));
  // }

  // getPages(offset: number, itemsPerPage: number, totalItems: number) {
  //   this.pager = this.pagerService.getPager(totalItems, offset, itemsPerPage, this.config.pageSize );
  // }
  // selectPage(page: number, event) {
  //   if (this.isValidPageNumber(page, this.pager.totalPages)) {
  //     this.pager.currentPage = page;
      
  //   console.log(this.pager);
  //     this.pageChange.emit(this.pager);
  //   }
  // }
  // isValidPageNumber(page: number, totalPages: number): boolean {
  //   return page > 0 && page <= totalPages;
  // }
  selectPageSize(){
    this.pager.pageSize = this.config.pageSize;
    this.setPage(this.pager.currentPage);
    this.pageChange.emit(this.pager);
    
  }



  @Input() config: PagerSetting = {};
  @Output() pagerObject = new EventEmitter<any[]>();
  count = 0;
  constructor(private pagerService: PagingService) { }


  // pager object
  pager: any = {};

  // paged items
  pagedItems: any[];
  ngOnInit() {   
    console.log(this.config);
    this.setPage(this.config.currentPage);
  }

  sendBackData() {
    this.count += 1;
    // this.returnData.emit(this.data.slice(0,20));
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
