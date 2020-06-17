import { Component } from "@angular/core";
import { AppList } from "src/app/app.list";
import { Router } from "@angular/router";
import { NgProgress } from "@ngx-progressbar/core";
import { ToastrService } from "ngx-toastr";
import { SortService } from "@services";
import { SettingRepo } from "@repositories";

@Component({
    selector: 'app-unlock-request',
    templateUrl: './unlock-request.component.html',
})
export class UnlockRequestComponent extends AppList {
    unlockRequests: any[] = [];

    constructor(
        private _router: Router,
        private _progressService: NgProgress,
        private _toastService: ToastrService,
        private _sortService: SortService,
        private _settingRepo: SettingRepo,) {
        super();
        this._progressRef = this._progressService.ref();
        this.requestList = this.searchUnlockRequest;
        this.requestSort = this.sortUnlockRequest;
    }

    ngOnInit() {
        this.headers = [
            { title: 'Subject', field: 'subject', sortable: true },
            { title: 'Request Date', field: 'requestDate', sortable: true },
            { title: 'Requester', field: 'requester', sortable: true },
            { title: 'Unlock Type', field: 'type', sortable: true },
            { title: 'Status', field: 'status', sortable: true },
            { title: 'Created Date', field: 'datetimeCreated', sortable: true },
        ];
        this.searchUnlockRequest();
    }

    onSearchUnlockRequest($event: any) {
        this.page = 1;
        this.dataSearch = $event;
        console.log($event);
        this.searchUnlockRequest();
    }

    searchUnlockRequest() {

    }

    sortUnlockRequest(sort: string): void {
        this.unlockRequests = this._sortService.sort(this.unlockRequests, sort, this.order);
    }
}