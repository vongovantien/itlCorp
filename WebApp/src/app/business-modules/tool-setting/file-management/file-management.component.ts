import { Component, OnInit } from '@angular/core';
import { SettingRepo } from '@repositories';
import { ToastrService } from 'ngx-toastr';
import { AppList } from 'src/app/app.list';

@Component({
    selector: 'app-file-management',
    templateUrl: './file-management.component.html',

})
export class FileManagementComponent extends AppList implements OnInit {
    constructor(
        private settingRepo: SettingRepo,
        private readonly _toastService: ToastrService,
    ) {
        super();
    }

    ngOnInit() {

    }

    onSelectTab(tabName: any) {

    }
    onSearchGroup() {

    }
}
