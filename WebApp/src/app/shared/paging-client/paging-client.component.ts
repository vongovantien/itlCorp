import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { PagingService } from './paging-client-service';

@Component({
  selector: 'app-paging-client',
  templateUrl: './paging-client.component.html',
  styleUrls: ['./paging-client.component.scss']
})
export class PagingClientComponent implements OnInit {

  @Input() data: any[];
  @Input() page_size: number;
  @Input() current_page: number;
  @Output() pagerObject = new EventEmitter<any[]>();
  count = 0;
  constructor(private pagerService: PagingService) { }

  // array of all items to be paged
  private allItems: any[];

  // pager object
  pager: any = {};

  // paged items
  pagedItems: any[];
  ngOnInit() {
    this.allItems = this.data;
    this.setPage(this.current_page);
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
    this.pager = this.pagerService.getPager(this.allItems.length, page, this.page_size);
    this.pagerObject.emit(this.pager);
  }
}
