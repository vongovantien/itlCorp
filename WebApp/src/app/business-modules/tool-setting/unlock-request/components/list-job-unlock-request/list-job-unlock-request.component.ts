import { Component, OnInit } from "@angular/core";
import { AppList } from "src/app/app.list";
import { SortService } from "@services";
import { ToastrService } from "ngx-toastr";

@Component({
    selector: 'list-job-unlock-request',
    templateUrl: './list-job-unlock-request.component.html',
})
export class UnlockRequestListJobComponent extends AppList implements OnInit {
    constructor(
        private _sortService: SortService,
        private _toastService: ToastrService
    ) {
        super();

    }

    ngOnInit(): void {
        this.headers = [
            { title: 'Unlock Name', field: 'unlockName', sortable: true },
            { title: 'Reason', field: 'reason', sortable: true }
        ];
    }
}