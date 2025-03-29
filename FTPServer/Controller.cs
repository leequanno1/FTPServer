using FTPServer.dto;
using FTPServer.dto.dbdto;
using FTPServer.dto.requests;
using FTPServer.dto.responses;
using lib;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FTPServer
{
    internal class Controller
    {
        private static string privateKey = "deCsf55CnPBMNOlqLyzlb+6w2Vud1dFWYRuG+q+bjJ2zZk2MDIJc12RJtjuCPvoZMtVy+dQ5MbrQnlnnYRUZls8+JBMyC4zHXzGIUBwBuLeLJ9a1VYWKsgs9UmiMit2lhJUg788Phvx04X5JCXP//reLY2WGVeJvR0hhtl8B2zIlUOjQOX3mIKKmZ+g7HOGk";

        private static FILE_SYSTEMEntities dbContext = new FILE_SYSTEMEntities();
        // authen
        public static void LoginController(Socket clientSocket, GlobalRequest globalRequest)
        {
            LoginRequest request = globalRequest.RequestObject as LoginRequest;
            User user = dbContext.Users.First(item => item.Username == request.Username && item.Password == request.Password);
            string id = user == null ? String.Empty : user.UserId;

            if (String.IsNullOrEmpty(id))
            {
                TcpProtocol.Send<GlobalResponse>(clientSocket, new GlobalResponse()
                {
                    AuthentToken = String.Empty,
                    Route = globalRequest.Route,
                    RequestObject = new LoginResponse() { Token = String.Empty }
                });
                return;
            }
            string token = TokenUltil.GenerateToken(id, privateKey);
            // send back to client
            TcpProtocol.Send<GlobalResponse>(clientSocket, new GlobalResponse()
            {
                AuthentToken = token,
                Route = globalRequest.Route,
                RequestObject = new LoginResponse() { Token = token }
            });
        }

        public static void SignUpController(Socket clientSocket, GlobalRequest globalRequest)
        {
            SignupRequest request = globalRequest.RequestObject as SignupRequest;
            // check if username is existed?
            if (UsernameExisted(request.Username))
            {
                TcpProtocol.Send<GlobalResponse>(clientSocket, new GlobalResponse()
                {
                    AuthentToken = globalRequest.AuthentToken,
                    Route = globalRequest.Route,
                    RequestObject = new SignupResponse()
                    {
                        Status = ResponseStatus.ACCOUNT_ALREADY_EXISTS,
                        Message = ResponseStatus.ACCOUNT_ALREADY_EXISTS_MESSAGE
                    }
                });
                return;
            }

            dbContext.Users.Add(new User()
            {
                UserId = GetId(),
                Username = request.Username,
                Password = request.Password,
            });

            dbContext.SaveChangesAsync();
            // send response
            TcpProtocol.Send<GlobalResponse>(clientSocket, new GlobalResponse()
            {
                AuthentToken = globalRequest.AuthentToken,
                Route = globalRequest.Route,
                RequestObject = new SignupResponse()
                {
                    Status = ResponseStatus.SUCCESS,
                    Message = ResponseStatus.SUCCESS_MESSAGE
                }
            });
        }

        // List
        public static void ListController(Socket clientSocket, GlobalRequest globalRequest)
        {
            if (!DoValidateToken<ListResponse>(clientSocket, globalRequest)) return;
            ListRequest request = globalRequest.RequestObject as ListRequest;
            string userId = globalRequest.AuthentToken.Split('.')[0];
            var compositeItems = dbContext.CompositeItems.Where(item => item.UserId == userId && item.ParentPath == request.FolderPath);
            List<CompositeItemDTO> folders = new List<CompositeItemDTO>();
            List<CompositeItemDTO> files = new List<CompositeItemDTO>();
            foreach (var item in compositeItems)
            {
                if (item.ItemType == CompositeConstance.FOLDER)
                {
                    folders.Add(new CompositeItemDTO(item));
                }
                else
                {
                    files.Add(new CompositeItemDTO(item));
                }
            }
            //Send back data
            TcpProtocol.Send<GlobalResponse>(clientSocket, new GlobalResponse()
            {
                Route = globalRequest.Route,
                AuthentToken = globalRequest.AuthentToken,
                RequestObject = new ListResponse()
                {
                    Files = files,
                    Folders = folders,
                }
            });
        }

        // Folder
        public static void AddFolder(Socket clientSocket, GlobalRequest globalRequest)
        {
            if (!DoValidateToken<FolderAddResponse>(clientSocket, globalRequest)) return;
            FolderAddRequest request = globalRequest.RequestObject as FolderAddRequest;
            int status = ResponseStatus.SUCCESS;
            string message = ResponseStatus.SUCCESS_MESSAGE;
            string userId = globalRequest.AuthentToken.Split('.')[0];
            if (!String.IsNullOrEmpty(FolderExisted($"{request.ParrentPath}/{request.FolderName}", userId)))
            {
                status = ResponseStatus.ERROR;
                message = ResponseStatus.ERROR_MESSAGE;
            }
            else
            {
                dbContext.CompositeItems.Add(new CompositeItem()
                {
                    ItemId = GetId(),
                    ItemPath = $"{request.ParrentPath}/{request.FolderName}",
                    ItemName = request.FolderName,
                    ItemType = CompositeConstance.FOLDER,
                    ParentPath = request.ParrentPath,
                    UserId = userId,
                });
                dbContext.SaveChangesAsync();
            }
            TcpProtocol.Send<GlobalResponse>(clientSocket, new GlobalResponse()
            {
                Route = globalRequest.Route,
                AuthentToken = globalRequest.AuthentToken,
                RequestObject = new FolderAddResponse()
                {
                    Status = status,
                    Message = message
                }
            });
        }


        public static void UpdateFolderName(Socket clientSocket, GlobalRequest globalRequest)
        {
            if (!DoValidateToken<FolderUpdateResponse>(clientSocket, globalRequest)) return;
            FolderUpdateRequest request = globalRequest.RequestObject as FolderUpdateRequest;
            int status = ResponseStatus.SUCCESS;
            string message = ResponseStatus.SUCCESS_MESSAGE;
            string userId = globalRequest.AuthentToken.Split('.')[0];
            string folderId = FolderExisted(request.FolderPath, userId);
            if (String.IsNullOrEmpty(folderId))
            {
                status = ResponseStatus.ERROR;
                message = ResponseStatus.ERROR_MESSAGE;
            }
            else
            {
                CompositeItem folder = dbContext.CompositeItems.First(item => item.ItemId == folderId);
                folder.ItemName = request.FolderName;
                folder.ItemPath = $"{folder.ParentPath}/{request.FolderName}";
                dbContext.SaveChangesAsync();
            }
            TcpProtocol.Send<GlobalResponse>(clientSocket, new GlobalResponse()
            {
                Route = globalRequest.Route,
                AuthentToken = globalRequest.AuthentToken,
                RequestObject = new FolderAddResponse()
                {
                    Status = status,
                    Message = message
                }
            });
        }

        public static void DeleteFolder(Socket clientSocket, GlobalRequest globalRequest)
        {
            if (!DoValidateToken<FolderDeleteResponse>(clientSocket, globalRequest)) return;
            FolderDeleteRequest request = globalRequest.RequestObject as FolderDeleteRequest;
            string userId = globalRequest.AuthentToken.Split('.')[0];
            int status = ResponseStatus.SUCCESS;
            string message = ResponseStatus.SUCCESS_MESSAGE;
            // find folder
            string folderId = FolderExisted(request.FolderPath, userId);
            
            if (String.IsNullOrEmpty(folderId))
            {
                status = ResponseStatus.ERROR;
                message = ResponseStatus.ERROR_MESSAGE;
            } else
            {
                // find all file and folder inside
                var composites = dbContext.CompositeItems.Where(item => item.ParentPath == request.FolderPath).ToList();
                // delete all inside item in server store
                DeleteItemInStore(composites);
                // delete all inside item in db
                dbContext.CompositeItems.RemoveRange(composites);
                // delete folder
                dbContext.CompositeItems.Remove(dbContext.CompositeItems.First(item => item.ItemId == folderId));
                dbContext.SaveChangesAsync();
            }
            // send response
            TcpProtocol.Send<GlobalResponse>(clientSocket, new GlobalResponse()
            {
                Route = globalRequest.Route,
                AuthentToken = globalRequest.AuthentToken,
                RequestObject = new FolderDeleteResponse()
                {
                    Status = status,
                    Message = message
                }
            });
        }

        // File
        public static void AddFile(Socket clientSocket, GlobalRequest globalRequest)
        {
            if (!DoValidateToken<FileAddResponse>(clientSocket, globalRequest)) return;
            FileAddRequest request = globalRequest.RequestObject as FileAddRequest;
            string userId = globalRequest.AuthentToken.Split('.')[0];
            // check if file existed
            string fileId = FileExisted($"{request.FolderPath}/{request.FileName}", userId);
            if (String.IsNullOrEmpty(fileId))
            {
                // generate file id id not exist.
                fileId = GetId();
            }
            Socket clientFileSocket = Server.GetFileTranferClientSocket(request.IpEndPoint);
            // send response ready to upload.
            TcpProtocol.Send<GlobalResponse>(clientSocket, new GlobalResponse()
            {
                AuthentToken = globalRequest.AuthentToken,
                Route = globalRequest.Route,
                RequestObject = new FileAddResponse()
                {
                    Status = ResponseStatus.READY,
                    Message = ResponseStatus.READY_MESSAGE
                }
            });
            // save file to server store
            HandleUploadFile(clientFileSocket, fileId, request.Size);
            // save file to server db
            dbContext.CompositeItems.AddOrUpdate(new CompositeItem()
            {
                ItemId = fileId,
                ItemPath = $"{request.FolderPath}/{request.FileName}",
                ParentPath = request.FolderPath,
                ItemName = request.FileName,
                UserId = userId,
                ItemType = CompositeConstance.FILE,
            });
            dbContext.SaveChanges();
        }

        public static void DownloadFile(Socket clientSocket, GlobalRequest globalRequest)
        {
            if (!DoValidateToken<FileUpdateResponse>(clientSocket, globalRequest)) return;
            FileDownloadRequest request = globalRequest.RequestObject as FileDownloadRequest;
            string userId = globalRequest.AuthentToken.Split('.')[0];
            // nhận thông tin file
            CompositeItem file = dbContext.CompositeItems.First(item => item.ItemPath == request.FilePath && item.UserId == userId);
            // lấy socket client
            Socket clientFileSocket = Server.GetFileTranferClientSocket(request.ClientEndpoint);
            FileInfo fileInfo = new FileInfo(StandardizeFilePath(file.ItemId));
            
            // gửi response sẳn sàng kết nối
            TcpProtocol.Send<GlobalResponse>(clientSocket, new GlobalResponse()
            {
                Route = globalRequest.Route,
                AuthentToken = globalRequest.AuthentToken,
                RequestObject = new FileDowloadResponse()
                {
                    Status = ResponseStatus.READY,
                    Message = ResponseStatus.READY_MESSAGE,
                    FileSize = fileInfo.Length
                }
            });
            // bắt đầu truyền file
            HandleDownloadFile(clientFileSocket, StandardizeFilePath(file.ItemId));
        }

        public static void UpdateFileName(Socket clientSocket, GlobalRequest globalRequest)
        {
            if (!DoValidateToken<FileUpdateResponse>(clientSocket, globalRequest)) return;
            FileUpdateRequest request = globalRequest.RequestObject as FileUpdateRequest;
            string userId = globalRequest.AuthentToken.Split('.')[0];
            string fileId = FileExisted(request.FilePath, userId);
            int status = ResponseStatus.SUCCESS;
            string message = ResponseStatus.SUCCESS_MESSAGE;
            if (String.IsNullOrEmpty(fileId))
            {
                status = ResponseStatus.NOT_FOUND;
                message = ResponseStatus.NOT_FOUND_MESSAGE;
            } else
            {
                // update db
                CompositeItem compositeItem = dbContext.CompositeItems.First(item => item.ItemId == fileId);
                compositeItem.ItemName = request.FileName;
                compositeItem.ItemPath = $"{compositeItem.ParentPath}/{request.FileName}";
                dbContext.SaveChangesAsync();
            }
            TcpProtocol.Send<GlobalResponse>(clientSocket, new GlobalResponse()
            {
                Route = globalRequest.Route,
                AuthentToken = globalRequest.AuthentToken,
                RequestObject = new FileUpdateResponse()
                {
                    Status = status,
                    Message = message
                }
            });

        }

        public static void DeleteFile(Socket clientSocket, GlobalRequest globalRequest)
        {
            if (!DoValidateToken<FileDeleteResponse>(clientSocket, globalRequest)) return;
            FileDeleteRequest request = globalRequest.RequestObject as FileDeleteRequest;
            string userId = globalRequest.AuthentToken.Split('.')[0];
            string fileId = FileExisted(request.FilePath, userId);
            int status = ResponseStatus.SUCCESS;
            string message = ResponseStatus.SUCCESS_MESSAGE;
            if (String.IsNullOrEmpty(fileId))
            {
                status = ResponseStatus.NOT_FOUND;
                message = ResponseStatus.NOT_FOUND_MESSAGE;
            }
            else
            {
                // update db
                dbContext.CompositeItems.Remove(dbContext.CompositeItems.First(item => item.ItemId == fileId));
                dbContext.SaveChangesAsync();
            }
            TcpProtocol.Send<GlobalResponse>(clientSocket, new GlobalResponse()
            {
                Route = globalRequest.Route,
                AuthentToken = globalRequest.AuthentToken,
                RequestObject = new FileDeleteResponse()
                {
                    Status = status,
                    Message = message
                }
            });
        }

        // private

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <returns>Return true if user existed</returns>
        private static bool UsernameExisted(string username)
        {
            User user = dbContext.Users.First(x => x.Username == username);
            return user != null;
        }

        private static bool DoValidateToken<TResoibseDTO>(Socket clientSocket, GlobalRequest glRequest)
        {
            if (TokenUltil.VerifyToken(glRequest.AuthentToken, privateKey))
            {
                return true;
            }
            else
            {
                TcpProtocol.Send<GlobalResponse>(clientSocket, new GlobalResponse()
                {
                    Route = glRequest.Route,
                    AuthentToken = String.Empty,
                    RequestObject = default(TResoibseDTO)
                });
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="userId"></param>
        /// <returns>Return folder id if existed else null</returns>
        private static string FolderExisted(String folderPath, string userId)
        {
            var folder = dbContext.CompositeItems.First(item => item.UserId == userId && item.ItemPath == folderPath && item.ItemType == CompositeConstance.FOLDER);
            return folder != null ? folder.ItemId : null;
        }

        private static string GetId()
        {
            return Guid.NewGuid().ToString("N");
        }

        private static void DeleteItemInStore(List<CompositeItem> composites)
        {
            // get realitic filePath
            var realiticFilePaths = new List<string>();
            foreach (var compositeItem in composites)
            {
                if(compositeItem.ItemType == CompositeConstance.FILE)
                {
                    realiticFilePaths.Add(StandardizeFilePath(compositeItem.ItemId));
                }
            }
            foreach(var path in realiticFilePaths)
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="userId"></param>
        /// <returns>Return file id if existed else null</returns>
        private static string FileExisted(string filePath, string userId)
        {
            var file = dbContext.CompositeItems.First(item => item.UserId == userId && item.ItemPath == filePath);
            return file != null ? file.ItemId : null;
        }

        private static void HandleUploadFile(Socket clientFileSocket, string fileId, long size)
        {
            FileTranferHelper.ReceiveFileFrom(clientFileSocket, CompositeConstance.ROOT_FOLDER_PATH, fileId);
            Server.RemoveSocket(clientFileSocket.ToString());
            clientFileSocket.Close();
        }

        private static void HandleDownloadFile(Socket clientFileSocket, string filePath)
        {
            FileTranferHelper.SendFileTo(clientFileSocket, filePath);
            while (clientFileSocket.Connected) { Thread.Sleep(100); }
            Server.RemoveSocket(clientFileSocket.ToString());
        }

        private static string StandardizeFilePath(string fileId)
        {
            return CompositeConstance.ROOT_FOLDER_PATH + fileId;
        }
    }
}
