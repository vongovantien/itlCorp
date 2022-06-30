import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';;
import { SettingRepo } from '@repositories';
import { ToastrService } from 'ngx-toastr';
import { catchError, finalize } from 'rxjs/operators';
import { AppList } from 'src/app/app.list';

export interface IFileItem { name: string, dateCreated: string, userModified: string, folderName: string, url: string }

@Component({
    selector: 'accouting-file-management',
    templateUrl: './accouting-file-management.component.html'
})

export class AccoutingFileManagementComponent extends AppList implements OnInit {
    itemsDefault: IFileItem[] = [{
        name: "SOA Folder",
        dateCreated: "19/03/2022",
        userModified: "22/03/2022",
        folderName: "SOA",
        url: ""
    },
    {
        name: "Settlement Folder",
        dateCreated: "19/03/2022",
        userModified: "22/03/2022",
        folderName: "Settlement",
        url: ""
    },
    {
        name: "Advance Folder",
        dateCreated: "19/03/2022",
        userModified: "22/03/2022",
        folderName: "Advance",
        url: ""
    }]

    isActiveClick: boolean = false;
    isActiveSearch: boolean = false;
    folderName: string;
    itemSelect: string;

    constructor(private _settingRepo: SettingRepo, private readonly _toastService: ToastrService, private _router: Router) {
        super();
    }


    ngOnInit() {
    }

    onGetFolerItems(data: IFileItem) {
        this.isActiveClick = !this.isActiveClick;
        this.folderName = data.folderName;
        this.dataSearch = { folder: data.folderName };
        this.getFolderFileManagement();
    }

    getFolderFileManagement() {
        this.isActiveSearch = true;
        this._settingRepo.getListFileManagement(this.page, this.pageSize, this.dataSearch)
            .pipe(catchError(this.catchError), finalize(() => { this.isLoading = false; this._progressRef.complete(); }),)
            .subscribe(
                (res: any) => {
                    this.totalItems = res.totalItems || 0;
                    this.itemsDefault = res.data || [];
                },
            );
    }

    onSelectFile(item: string) {
        this.itemSelect = item;
    }
    onRedirectLink(item: string) {
        window.open(`${item}`, "_blank");
    }

    onSearchValue(event: { field: string; searchString: any; }) {
        this.dataSearch = event;
        this.getFolderFileManagement();
    }
}
