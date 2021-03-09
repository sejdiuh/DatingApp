import { Component, OnInit, Input } from '@angular/core';
import { Message } from 'src/app/_models/Message';
import { UserService } from 'src/app/_services/user.service';
import { AuthService } from 'src/app/_services/auth.service';
import { AlertifyService } from 'src/app/_services/alertify.service';
import { tap } from 'rxjs/operators';

@Component({
  selector: 'app-member-message',
  templateUrl: './member-message.component.html',
  styleUrls: ['./member-message.component.css']
})
export class MemberMessageComponent implements OnInit {
  @Input() recipientId: number;
  messages: Message[];
  newMessage: any = {};
  constructor(private userService: UserService,
              private authService: AuthService,
              private alertify: AlertifyService) { }

  ngOnInit() {
    this.loadMessage();
  }

  loadMessage() {
    const currentUserId = +this.authService.decodetToken.nameid;
    this.userService.getMessageThread(this.authService.decodetToken.nameid, this.recipientId)
                      .pipe(
                         tap(messages => {
                           for ( let i = 0; i < messages.length; i++) {
                              if (messages[i].isRead === false && messages[i].recipientId === currentUserId) {
                                this.userService.markAsRead(currentUserId, messages[i].id);
                              }
                           }
                         })
                      )
                      .subscribe(message => {
                        this.messages = message;
                      }, error => {
                        this.alertify.error(error);
                      });
  }
  sendMessage() {
    this.newMessage.recipientId = this.recipientId;
    this.userService.sendMessage(this.authService.decodetToken.nameid, this.newMessage)
                      .subscribe((message: Message) => {
                          this.messages.unshift(message);
                          this.newMessage.content = '';
                      }, error => {
                          this.alertify.error(error);
                      });
  }
}