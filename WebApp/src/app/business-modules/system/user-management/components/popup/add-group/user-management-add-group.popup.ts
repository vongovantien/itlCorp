import { Component, OnInit, Input } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';
import { SystemRepo } from '@repositories';
import { Observable } from 'rxjs';
import { Group } from '@models';

@Component({
    selector: 'usermanagement-add-group-popup',
    templateUrl: './user-management-add-group.popup.html'
})

export class UserManagementAddGroupPopupComponent extends PopupBase implements OnInit {
    @Input() userId: string;

    groups: Observable<Group[]>;
    selectedGroup: Group;
    selectedPosition: CommonInterface.INg2Select;

    positions: CommonInterface.INg2Select[] = [
        { id: 'Manager-Leader', text: 'Manager-Leader' },
        { id: 'Deputy', text: 'Deputy' },
        { id: 'Assistant', text: 'Assistant' },
        { id: 'Staff', text: 'Staff' },
    ];

    constructor(
        private _systemRepo: SystemRepo
    ) {
        super();
    }

    ngOnInit() {
        this.groups = this._systemRepo.getListGroup();
    }

    onSelectGroup(group: Group) {
        this.selectedGroup = group;
    }

    onSaveGroup() {
        this.isSubmitted = true;
        if (!this.selectedPosition || !this.selectedGroup) {
            return;
        }
        console.log(this.selectedGroup, this.selectedPosition);

        const body = {
            userId: this.userId,
            groupId: this.selectedGroup.id,
            departmentId: this.selectedGroup.departmentId,
            officeId: this.selectedGroup.officeId,
            companyId: this.selectedGroup.companyId,
            position: this.selectedPosition,
            active: true
        };
        console.log(body);
        // this._systemRepo.addUserToGroupLevel(body).subscribe(
        //     (res) => {
        //         console.log(res);
        //     }
        // );
    }
}
